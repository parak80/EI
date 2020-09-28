/* SystemJS module definition */
declare var module: NodeModule;
interface NodeModule {
  id: string;
}

/* Bootstrap plugins*/
interface JQuery {
  popover(options?: any) : any;
}


declare module "*.json" {
  const value: any;
  export default value;
}

